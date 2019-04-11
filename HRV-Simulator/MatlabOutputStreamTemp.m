close all
clc
clear

%% instantiate the library
cf = pwd; %get current folder
addpath(genpath(strcat(cf,'/LSL2Unity/LSLWin'))); %add path that contains libraries
disp('Loading library...');
lib = lsl_loadlib();

% make a new stream outlet
disp('Creating a new streaminfo...');
info = lsl_streaminfo(lib,'PPG','HRV',8,100,'cf_float32','sdfwerr32432');

disp('Opening an outlet...');
outlet = lsl_outlet(info);

disp('Now transmitting data...');
a = 0.2;
i = 0;

while true
    pause(0.2);
    % fake HRV data
    %data = 60 + 5 * sin(a*i);
    data = 100 + 50 * sin(a*i);
    disp(data);
    outlet.push_sample(data);
    i = i+1;
end