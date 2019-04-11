%% Reading data in rolling windows and calculating HRV metrics from
%  raw data from a PPG signal must be in a variable "datarawPPG" (2xN), with the first
%  row for the PPG data and the second one for the timestamps.

close all
clc
clear
% Data(1,:)=PPGdata;
% Data(2,:)=timestamp;
load('PPGdata/APdatab3.mat');
Data = dataraw;
% datarawPPG=Data;    %for the test on previously saved data
% clear dataraw datanorm
%% Features of data (These variables should be defined in the data Acq)
fs = 200;  % Sampling frequency (Hz) of the pulse data
Te = 1;    % Rolling (non-overlapping) time
           % (Execution time (s) for processing)
Nw=30/Te;     % Window size. Must be a multiple of Te
% Tf = (20*60)/Te-Nw;   % Final time of the Data
Tf = 0;
while (Data(1,Tf+1)~=0)
    Tf = Tf+1;
end
Tf = Tf/fs;
ChECG=1;  % Channel of interest in the OpenBCI
% Memory Allocation
siz=zeros(1,Nw+1);
SDNN=[];
RMSSD=[];
HR=[];

% First valid window (the first Nw seconds of data are saved in the array
    spw=fs*Nw*Te;  % samples per window
    spr=fs*Te;  % samples per roll
    for i=1:Nw
        %tic
        chunk = Data(1,(i-1)*spr+1:i*spr); %First data chunk from raw data
        stamps = Data(2,(i-1)*spr+1:i*spr); %Time stamps correspondingly
        tam=size(chunk);
        siz(i)=tam(2); %the size of chunk is 200
        %normalize the pulse signal every Ts seconds.
        MAX=max(chunk(ChECG,1:siz(i)));
        chunkn(ChECG,1:siz(i))=chunk(ChECG,1:siz(i))/MAX; % normalized chunk, dividing the original data by the maximum
        if i==1
            Data(1,1:siz(i))=chunk(ChECG,1:siz(i));
            Data(2,1:siz(i))=stamps(1,1:siz(i));
            datanorm(1,1:siz(i))=chunkn(ChECG,1:siz(i));
        else
            Data(1,sum(siz(1:i-1))+1:sum(siz(1:i)))=chunk(ChECG,1:siz(i));
            Data(2,sum(siz(1:i-1))+1:sum(siz(1:i)))=stamps(1,1:siz(i));
            datanorm(1,sum(siz(1:i-1))+1:sum(siz(1:i)))=chunkn(ChECG,1:siz(i));
        end
    end
%Peak detection and HRV metrics calculation per window
    it=1;
    over=0;
    thres=0.6;
    tEnd = 0;
    statWindow = 60;
    while over==0
        [Mag,Pk] = findpeaks(datanorm(1,1:sum(siz(1:Nw))),'MinPeakHeight',thres,'MinPeakDistance',0.3*fs); %minpeak = 80 with abs(data2)
        pkdif = diff(Pk)/fs*1000;   %Interbeat vector in ms
        sqpkdif=diff(pkdif).^2; %Square differences in successive intervals
%verify peak detection (to see if the detection is ok)
%         if it==40
%             figure(1)
%             subplot(2,1,1)
%             plot(dataraw(1,1:sum(siz(1:Nw))))
%             subplot(2,1,2)
%             plot(datanorm(1,1:sum(siz(1:Nw))))
%             hold on
%             scatter(Pk,datanorm(1,Pk))
%             hold on
%         end

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
%% TODO
% Fix NaN issue.
    

