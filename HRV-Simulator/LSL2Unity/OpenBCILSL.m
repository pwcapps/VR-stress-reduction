%%% Code to read OpenBCI sensor data from LSL stream and compute control
%%% input. Uncomment figures to obtain real time plots. Include LSL libraries
%%% (liblsl folder) in the same folder where this file is being run
clc; clear all; close all;
cf = pwd; %get current folder
addpath(genpath(strcat(cf,'/liblsl'))); %add path that contains libraries
%% Configure data ACQ with LSL protocol from OpenBCI App
% Instantiate the library
disp('Loading the library...');
lib = lsl_loadlib();
% Resolve a stream...
disp('Resolving a data stream...');
result = {};
while isempty(result)
    result = lsl_resolve_byprop(lib,'type','EEG'); 
end
% Create a new inlet
disp('Opening an inlet...');
inlet = lsl_inlet(result{1});  % Ready to read data from Ganglion
% Features of data
fs = 200;  % Sampling frequency (Hz) of the ECG data
Te = 1;    % Control input sample time (s)
Rw = 30; %Rolling window length in sec
Nw = floor(Rw/Te);   % Number of windows for HRV computation
ChECG = 1; % Channel of ECG data

%% Reading and processing cycle
% First refernce time stamp (without data)
[chunk,stamps] = inlet.pull_chunk();
pause(Te);

% Cycle to obtain the first valid window 
for i=1:Nw %while true
    % get chunk from the inlet
    tic
    [chunk,stamps] = inlet.pull_chunk();
    tam=size(chunk);
    siz(i)=tam(2);
    datatemp(i,1:siz(i))=chunk(ChECG,1:siz(i));
    stamptemp(i,1:siz(i))=stamps(1,1:siz(i));
    if i==1
        dataraw(1,1:siz(i))=chunk(ChECG,1:siz(i));
        dataraw(2,1:siz(i))=stamps(1,1:siz(i));
    else
        dataraw(1,sum(siz(1:i-1))+1:sum(siz(1:i)))=chunk(ChECG,1:siz(i));
        dataraw(2,sum(siz(1:i-1))+1:sum(siz(1:i)))=stamps(1,1:siz(i));
    end
%     figure(1)
%     clf
%     plot(dataraw(1,1:sum(siz(1:Nw))),'Color','blue');
%     i
    toc
    pause(Te-toc);
end

% HRV calculation and control
it=1;
t= linspace(0, Te, Te*8192); % time to sound the tone
while true
   tic
   [Mag,Pk] = findpeaks(dataraw(1,1:sum(siz(1:Nw))),'MinPeakHeight',400,'MinPeakDistance',0.3*fs); %minpeak = 80 with abs(data2)
   pkdif = diff(Pk);
   hr(it) = 1/mean(pkdif/fs)*60;
   hrv(it) = std(pkdif);
   
%    figure(2)
%    clf
%    subplot(3,1,1)
%    plot(dataraw(1,1:sum(siz(1:Nw))),'Color','blue');
%    hold on
%    plot(Pk,dataraw(1,Pk),'k^','markerfacecolor',[1 0 0]);
%    title('Pulse signal and detected peaks')
%    subplot(3,1,2)
%    stem(hrv)
%    hold on
%    title('Heart Rate Variability')
%    subplot(3,1,3)
%    stem(hr)
%    hold on
%    title('Heart Rate (bpm)')
   
% Change the pitch (frequency) of a piece
   %tone=400/hrv(it)*3;
   Ref = 20;
   P = 1;
   tone=(hrv(it)-Ref)*P+300;
   signal=sin(2*pi*tone*t);
   sound(signal);

% Get new chunk from the inlet
   [chunk,stamps] = inlet.pull_chunk();
   tam=size(chunk);
   siz(Nw+1)=tam(2);
   
% Actualize window (discard the oldest)   
    dataraw(1,sum(siz(1:Nw))+1:sum(siz(1:Nw+1)))=chunk(ChECG,1:siz(Nw+1));
    dataraw(2,sum(siz(1:Nw))+1:sum(siz(1:Nw+1)))=stamps(1,1:siz(Nw+1));
    dataraw=(circshift(dataraw',-siz(1)))';
    siz=(circshift(siz',-1))';   

    it=it+1;
    pause(Te-toc)
    
end
    