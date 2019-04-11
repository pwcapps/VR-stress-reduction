clc;
clear;
load('PPGdata/APdata7.mat');
% n is the index of chunk from 1 to 10.
n = 4;
% Extracts the nth chunk into sample data.
sampleData = dataraw(1,12000*(n-1)+1:12000*(n)); 

Fs= 200; % Sample frequency

t = linspace(0,60,12000); % Convert sample count into second
t2 = 1:12000; % Sample count

lowpassSampleData = filter(lowpass2,sampleData); % Choose lowpass2 as the filter

% Finds the peak in each time interval of 0.65 seconds. This setting needs
% to be changed be adaptive in real time. Suppose the heart rate is n
% beats/minute, the methods has better performance if 0.65 is set to
% 0.025n.
[pks,locs] = findpeaks(lowpassSampleData,Fs,'MinPeakDistance',0.65);

% By looking at the raw data, I noticed some calibration were performed by
% PPG device during measurements: When the signal is far from 0, it will
% calibrate the signal by lifting or lowering the following data to
% somewhere near 0. This has caused some problems to the detection of heart
% beats. This method is trying to eliminate the calibrations by first
% recongizing and then recover the raw data. NOT WORKING PERFECTLY NOW.
calibRemoved = sampleData;
modification = double.empty(0); % Create an empty array to hold the loacations of modifications
stage = 0; % The value to be added to the following data
adjacentDerivative1 = 0; % Average derivative of (3) former data
adjacentDerivative2 = 0; % Average derivative of (3) latter data
for i = 1:12000-3
    if abs(calibRemoved(i)-calibRemoved(i+1))>1000 % If a sudden change is detected
         adjacentDerivative1 = ((calibRemoved(i-2)-calibRemoved(i-1))+(calibRemoved(i-1)-calibRemoved(i)))/2;
         adjacentDerivative2 = ((calibRemoved(i+1)-calibRemoved(i+2))+(calibRemoved(i+2)-calibRemoved(i+3)))/2;
         if abs(adjacentDerivative1-adjacentDerivative2)<50 % If the former and latter data are about to have the same derivative
            stage = calibRemoved(i)-calibRemoved(i+1);
            modification = [modification,i];
        end
    end
    calibRemoved(i+1:end) = calibRemoved(i+1:end)+stage;
    stage = 0;
end
modificationValue = calibRemoved(modification);
modification=modification./fs;
lowpassCalibRemovedData = filter(lowpass2,calibRemoved);
[pks2,locs2] = findpeaks(lowpassCalibRemovedData,Fs,'MinPeakDistance',0.65);

% Eliminates the upward or downward trend in a large interval. A typical
% sample is between 7500 and 8000 samples in data7, where most values are
% below average. This method is implemented by first creating a smoothed
% version of the original data, and use the original data to substract the
% smoothed data. Then we can get the centered data without trend while
% still keeping details.
sm = smooth(sampleData,151);
centered(1,12000) = 0;
for i = 1:12000
    centered(i) = sampleData(i) - sm(i);
end
lowpassCenteredData = filter(lowpass2,centered);
[pks3,locs3] = findpeaks(lowpassCenteredData,Fs,'MinPeakDistance',0.65);

figure(1)
subplot(311)
plot(t,sampleData,t,lowpassSampleData,locs,pks,'o');
title('Original');
subplot(312)
p1 = plot(t,calibRemoved,t,lowpassCalibRemovedData,locs2,pks2,'o');
hold on
p2 = plot(modification,modificationValue,'*');
hold off
if p2.isvalid>0 % If there is any modification
    legend(p2,'removed calibration');
end
title('Calibration Removed');
subplot(313)
plot(t,centered,t,lowpassCenteredData,locs3,pks3,'o');
title('Centered')

figure(2) % Plot the heart beat rate based on Filtered ORIGINAL data
% Calculate intervals between each two peaks
for i = 2:length(locs)
    intervals(i) = locs(i)-locs(i-1);
end
% Calculate heart rate beased on one previous interval
for i = 1:length(intervals)
    heartRate1(i) = 60/intervals(i);
end
heartRate1(1)=0;
% Calculate heart rate based in three previous intervals
for i = 3:length(intervals)
    heartRate2(i) = 60/intervals(i)/3+60/intervals(i-1)/3+60/intervals(i-2)/3;
end
heartRate2(1:3)=0;
plot(locs,heartRate1,locs,heartRate2);
title('Heart Rate vs. Time');
xlabel('Seconds');
ylabel('Beats/Minute');
legend('1 sample','moving avg of 3 samples');

% figure(3)
% stem(t2(6401:6405),calibRemoved(6401:6405));
%                 ip3 = calibRemoved(6404)








