%% import sample data
close all
clc
clear
% choose one sample data form PPGdata
load('PPGdata/APdatab7.mat');
timeOffset = dataraw(2,1);
dataraw(2,:)=dataraw(2,:)-timeOffset;

ppg = [];
ts=[];
dataAutosense = struct('ppg',ppg,'timestamp',ts);

% pretend to connect with the watch :)
% pause(2);
% fprintf('Waiting for connection\n');
% pause(2)
% fprintf('Connection Found!\n');

%% instantiate the library
cf = pwd; %get current folder
addpath(genpath(strcat(cf,'/LSL2Unity/LSLWin'))); %add path that contains libraries
disp('Loading library...');
lib = lsl_loadlib();

% make a new stream outlet
disp('Creating a new streaminfo...');
info = lsl_streaminfo(lib,'BioSemi','EEG',8,100,'cf_float32','sdfwerr32432');

disp('Opening an outlet...');
outlet = lsl_outlet(info);

%% 
figure;
tic;
time1 = 0;
pos1 = 1;
time2 = 0;
pos2 = 1;

pause(10);
while true

fs = 200;  % Sampling frequency (Hz) of the pulse data
Te = 1;    % Rolling (non-overlapping) time
           % (Execution time (s) for processing)
% Memory Allocation
siz=zeros(1,Nw+1);
SDNN=[];
RMSSD=[];
HR=[];

% First valid window (the first Nw seconds of data are saved in the array
    spw=fs*Nw*Te;  % samples per window
    spr=fs*Te;  % samples per roll

    thres=0.6;
    tEnd = 0;
    statWindow = 60;
        [Mag,Pk] = findpeaks(datanorm(1,1:sum(siz(1:Nw))),'MinPeakHeight',thres,'MinPeakDistance',0.3*fs); %minpeak = 80 with abs(data2)
        pkdif = diff(Pk)/fs*1000;   %Interbeat vector in ms
        sqpkdif=diff(pkdif).^2; %Square differences in successive intervals

% Time-based measures for HRV (one measure each roll in window)
        HR(it) = 1/mean(pkdif/1000)*60;  %heart rate in bpm
        SDNN(it) = std(pkdif);   % Standard Deviation of NN distance (ms)
        RMSSD(it)=sqrt(mean(sqpkdif)); % RMS of successive differences (ms)

        figure(1)
        subplot(4,1,1);
        plot(Data(1,1:sum(siz(1:Nw))))
        sub2 = subplot(4,1,2);
        plot(datanorm(1,1:sum(siz(1:Nw))))
        hold on
        scatter(Pk,datanorm(1,Pk))
        if tEnd<statWindow
            tEnd = length(HR);
        end
        t = 1:tEnd;
        subplot(4,1,3);
        plot(t,HR((length(HR)-tEnd+1):length(HR)));
        title('Heart rate');
        xlabel('Seconds');
        subplot(4,1,4);
        plot(t,SDNN((length(SDNN)-tEnd+1):length(SDNN)));
        title('HRV(SDNN)');
        xlabel('Seconds');
        pause(0.5);
        cla(sub2);
        
%Time-based measures for HRV (one measure each roll in window)
        HR(it) = 1/mean(pkdif/1000)*60;  %heart rate in bpm
        SDNN(it) = std(pkdif);   % Standard Deviation of NN distance (ms)
        RMSSD(it)=sqrt(mean(sqpkdif)); % RMS of successive differences (ms)
%
% Actualize window (discard the oldest values)   
    % Get new chunk from the inlet
        chunk = Data(1,spw+(it-1)*spr+1:spw+it*spr);
        stamps = Data(2,spw+(it-1)*spr+1:spw+it*spr);
        tam=size(chunk);
        siz(Nw+1)=tam(2);
        MAX=max(chunk(ChECG,1:siz(Nw+1))); %normalizing the pulse signal
        chunkn(ChECG,1:siz(Nw+1))=chunk(ChECG,1:siz(Nw+1))/MAX;
    % Actualize window (discard the oldest)   
        Data(1,sum(siz(1:Nw))+1:sum(siz(1:Nw+1)))=chunk(ChECG,1:siz(Nw+1));
        datanorm(1,sum(siz(1:Nw))+1:sum(siz(1:Nw+1)))=chunkn(ChECG,1:siz(Nw+1));
        Data(2,sum(siz(1:Nw))+1:sum(siz(1:Nw+1)))=stamps(1,1:siz(Nw+1));
        Data=(circshift(Data',-siz(1)))';
        datanorm=(circshift(datanorm',-siz(1)))';
        siz=(circshift(siz',-1))';  
        
% Increase iterations and exit    
    it=it+1;
    if it==Tf
        over=1;
    end
    end
    

%     dataAutosense.ppg = [];
    
    time1 = time2;
    pos1 = pos2;

%     if(length(dataAutosense.ppg)>1200)
%         plot(dataAutosense.ppg(end-1200:end));
%         % the ploting is taking extra time, stop it when you know you are
%         % getting the correct data.
%         pause(0.1)
%     end

    
%% Transmit data

% data = double();
% data = dataAutosense.ppg(end);
% send data into the outlet, sample by sample

disp('Now transmitting data...');
data

outlet.push_sample(data);

end

   
