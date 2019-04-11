%%% Code to read OpenBCI sensor data from .txt file and compute control input 
%%% This file must be run from the folder that contains FileName_ECG
clc; clear all; close all;
FileName_ECG = 'OpenBCI-RAW-ecg1.txt';
readFormat = '%*d %*f %*f %*f %f %*f %*f %*f %*{hh:mm:ss.SSS}T'; %only read column 2 (channel 1)
fs = 200; %sampling frequency (Hz)
Te = 1; %control input sample time
Rw = 30; %Rolling window length in sec
Nw = floor(Rw/Te); %Rolling window in samples
k=1;
HRV = 0;
t = linspace(0,Te,Te*8192);
Ref = 12;
Kp = 1; Ki = 0; Kd = 0; Kn = 0; %controller gains
I = 0; Di = 0; D = 0; u = 0; 
while true
    if exist(FileName_ECG, 'file') == 2       
    %% reading samples Sensor 
        tic
        k = k+1
        fid= fopen(FileName_ECG,'r');
        C = textscan(fid,readFormat,'Delimiter',',','CommentStyle','%'); %Read all data
        fclose(fid);
        A_Ecg = C{1};
        %% Compute HRV         
        data = A_Ecg(max(1,end-fs*Nw*Te):end); %Obtain data for current window   
        [~,Pk] = findpeaks(data.^2,'MinPeakHeight',5e2,'MinPeakDistance',0.3*fs); %MinPeakHeight for Pulse: 5e2, gel: 5e2. Check before running 
        pkdif = diff(Pk);
        hrv(k) = std(pkdif);       
        %% Compute control input with PID
        E = (hrv(k)-Ref); %error
        I(k) = I(k-1)+Ki*E;
        P(k) = Kp*E;
        Di(k) = Di(k-1)+D(k-1);
        D(k) = Kn*(Kd*E-Di(k));
        u(k) = P(k)+I(k)+D(k);    
        tone=u(k)+300;
        signal=sin(2*pi*tone*t);
        sound(signal);
        pause(max(Te-toc,0));
    else
        break
    end
end


