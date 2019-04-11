%% MUSIC MODULATION CODE
clear all
clc
frameLength = 44100;  % Number of samples to be streamed or played each step time
%frameLength = 2^11;  % Number of samples to be streamed or played each step time

% Creating the object fileReader with the name of the file.  It detects the
% sample frequency of the file (depends on the quality or resolution of the audio file) 

fileReader = dsp.AudioFileReader(...
    'Chopin - Nocturne op.9 No.2.mp3',...
    'SamplesPerFrame',frameLength);

% Object for the output device.  The SampleRate could be changed to play the file slow or fast.

deviceWriter = audioDeviceWriter('SampleRate',fileReader.SampleRate);

% This is an object to visualize the audio signal on time (there is another
% for the spectrum).
scope = dsp.TimeScope(...
    'SampleRate',fileReader.SampleRate,...
    'TimeSpan',16,...
    'BufferLength',1.5e6,...
    'YLimits',[-1,1]);

Fs=fileReader.SampleRate;       % Sampling Frequency of the file
t= linspace(0, frameLength/Fs, frameLength);

FLANGER = audioexample.Flanger;
PITCH = audiopluginexample.PitchShifter;
BASS = audiopluginexample.BassEnhancer;

%% Configure data ACQ with LSL protocol from OpenBCI App
cf = pwd; %get current folder
addpath(genpath(strcat(cf,'/LSLMac')));
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
Ref = 60;
P = 0.5;
%%
i=1; %Only for counting :)
while ~isDone(fileReader)    
    tic % For measuring execution time
    signal = step(fileReader);  % Getting the signal from the object fileReader. signal is an array with the number of columns = number of audio channels
    % signal(1:10,:)
    
    %%
    C = inlet.pull_chunk();
    A_Ecg = [A_Ecg; C(ChECG,:)'];
    %% Compute HRV         
     data = A_Ecg(max(1,end-fs*Nw*Te):end); %Obtain data for current window   
     [~,Pk] = findpeaks(data.^2,'MinPeakHeight',3e6,'MinPeakDistance',0.3*fs); %MinPeakHeight for Pulse: 5e2, gel: 5e2. Check before running 
     pkdif = diff(Pk);
     hrv(k) = std(pkdif); 
     hr(k) = 1/mean(pkdif/fs)*60; %Heart rate in bpm
    %% Compute control input
    tone(k)=(hr(k)-Ref)*P;
  
    
    PITCH.PitchShift = -tone(k);
    PITCH.Overlap = 0.167;
    BASS.BPUpperCutoff = 100;
    BASS.Gain = 0.1;
    FLANGER.FeedbackLevel=0.8;
    signal=step(PITCH,signal);
    %signal=step(FLANGER.signal);
    signal=step(BASS,signal);
    %step(scope,signal);  % Just plotting signal with the object "scope"
    play(deviceWriter,signal);  % Playing the signal to the output device.  In this case it is the one used by the Laptop.
            
    
    k = k+1
    pause(max(Te-toc,0));
    
    %pause(1)
    toc
    i=i+1;%
end                                         

% 
% data2 = data.^2;
% figure
% plot(data2)
% hold on
% scatter(Pk,data2(Pk))

release(fileReader);                  
release(deviceWriter);   
delete(deviceWriter);
delete(fileReader); 
delete(inlet);
clear inlet deviceWriter fileReader


save('HR_Data.mat')


