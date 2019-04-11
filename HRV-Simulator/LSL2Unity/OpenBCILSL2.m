%%% Code to read OpenBCI sensor data from LSL stream and compute control
%%% input. Same code as OpenBCILSL file, with similar control flow as  
%%% OpenBCI.m. Include LSL libraries (liblsl folder) in the same folder 
%%% where this file is being run
clc; clear all; close all;
cf = pwd; %get current folder
addpath(genpath(strcat(cf,'/LSLWin'))); %add path that contains libraries
%% Configure data ACQ with LSL protocol from OpenBCI App
disp('Loading the library...');
lib = lsl_loadlib();
disp('Resolving a data stream...');
result = {};
while isempty(result)
    result = lsl_resolve_byprop(lib,'type','EEG'); 
end
disp('Opening an inlet...');
inlet = lsl_inlet(result{1});  % Ready to read data from Ganglion in Channel 1
% Features of data
fs = 200;  % Sampling frequency (Hz) of the ECG data
Te = 1;    % Control input sample time (s)
Rw = 30; %Rolling window length in sec
Nw = floor(Rw/Te);   % Number of windows for HRV computation
ChECG = 1; % Channel of ECG data
%% Reading and processing cycle
C = inlet.pull_chunk();
pause(Te)
A_Ecg=C(ChECG,:)';
k=1;
t = linspace(0,Te,Te*8192);
Ref = 12;
P = 1;
while true    
%% reading samples Sensor
tic
    C = inlet.pull_chunk();
    A_Ecg = [A_Ecg; C(ChECG,:)'];
    %% Compute HRV         
     data = A_Ecg(max(1,end-fs*Nw*Te):end); %Obtain data for current window   
     [~,Pk] = findpeaks(data.^2,'MinPeakHeight',4e2,'MinPeakDistance',0.3*fs); %MinPeakHeight for Pulse: 5e2, gel: 5e2. Check before running 
     pkdif = diff(Pk);
     hrv(k) = std(pkdif); 
     hr(k) = 1/mean(pkdif/fs)*60; %Heart rate in bpm
    %% Compute control input
    tone=(hrv(k)-Ref)*P+300;
    signal=sin(2*pi*tone*t);
    sound(signal);
    k = k+1
    pause(max(Te-toc,0));
end

