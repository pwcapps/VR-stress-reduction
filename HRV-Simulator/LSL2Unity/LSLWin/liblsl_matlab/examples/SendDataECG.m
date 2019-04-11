%% instantiate the library
disp('Loading library...');
lib = lsl_loadlib();

% make a new stream outlet
disp('Creating a new streaminfo...');
info = lsl_streaminfo(lib,'BioSemi','EEG',8,100,'cf_float32','sdfwerr32432');

disp('Opening an outlet...');
outlet = lsl_outlet(info);

% send data into the outlet, sample by sample
disp('Now transmitting data...');
for i=1:59999
    %outlet.push_sample([sin(2*pi*1/200*i);0;0;0;0;0;0;0]);
    outlet.push_sample([ECGtot(i);0;0;0;0;0;0;0]);
    pause(0.005);
end