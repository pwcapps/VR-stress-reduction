%%% Code for reading data file in hard drive. Currently it is reading the
%%% entire file on each iteration, but can be improved to extract only the
%%% needed data.

clc;
clear all;
close all;

FileName_ECG = 'OpenBCI-RAW-ecg22.txt';
%FileName_HRV = 'HRV.txt';
readFormat = '%d %f %*f %*f %*f %*f %*f %*f %*{hh:mm:ss.SSS}T';

Data=[];
Data_e=[];
pos = 0;
k = 1;
hl = 0;
while true
    if exist(FileName_ECG, 'file') == 2
        
    %% reading samples from ECG sensor
        %%%%Textscan alternative: works good but reads entire file on every 
        %%%% iteration. Very choppy when use of header lines to trim data.
        %%%% 
        tic
        fid= fopen(FileName_ECG,'r');
        %fseek( fid  , pos(k) ,'bof' ) ;
        [C,pos(k+1)] = textscan(fid,readFormat,'Delimiter',',','CommentStyle','%'); %'CommentStyle','%' 'HeaderLines',hl(k)+6
        fclose(fid);
        A_Ecg = C{1};
        E_Ecg=C{2};
        hl(k+1) = length(A_Ecg);
        t1(k) = toc;
        
        %brings all past data since the experiment began

        pause(1-t1(k)); 
        k=k+1

        
    else
        break
    end
end


