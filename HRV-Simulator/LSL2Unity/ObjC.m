clc; clear all; close all;
FileName_EEG = 'BandPowerValue.csv';
E=[];
while true
    if exist(FileName_EEG, 'file') == 2       
    %% reading samples Sensor
        tic
        D=csvread(FileName_EEG,1,1);
        D(:,6)=[];
        [i j]=size(D);
        M=floor(i/14);
        if size(E)==size(D)
            break
        else
        for a=1:M
            F(a,:)=[D(1+(a-1)*14,:) D(2+(a-1)*14,:) D(3+(a-1)*14,:) D(4+(a-1)*14,:) D(5+(a-1)*14,:) D(6+(a-1)*14,:) D(7+(a-1)*14,:) D(8+(a-1)*14,:) D(9+(a-1)*14,:) D(10+(a-1)*14,:) D(11+(a-1)*14,:) D(12+(a-1)*14,:) D(13+(a-1)*14,:) D(14+(a-1)*14,:) ];
        end
        pause(1-toc)
        E=D;
        end
        else
        break
    end
end