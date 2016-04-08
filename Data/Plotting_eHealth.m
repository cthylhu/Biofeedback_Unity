clear; clear all;close all;
%%
% load E-health sensor data
%eHealthECG = load('EHealth_rawECG.txt');  %ECG raw data
delimiterIn = ',';
headlinesIn = 0;

eHealthECG = importdata('EHealth_rawECG.txt', delimiterIn);
eHealthGSR = importdata('EHealth_GSR.txt', delimiterIn);   % GSR from E-health platform
eHealthHRV = importdata('EHealth_HRV.txt', delimiterIn);
eHealthHB = importdata('EHealth_hrBeat.txt', delimiterIn);  % calculated Heart Beat Rate

%{
eHealthECG = importdata('EHealth2_rawECG.txt', delimiterIn);
eHealthGSR = importdata('EHealth2_GSR.txt', delimiterIn);   % GSR from E-health platform
eHealthHRV = importdata('EHealth2_HRV.txt', delimiterIn);
eHealthHB = importdata('EHealth2_hrBeat.txt', delimiterIn);  % calculated Heart Beat Rate
%}

%eHealthGSR = load('EHealth_GSR.txt');     
%eBreathing = load('Breathing.txt'); % breathing raw data
%eBR = load('BR.txt');               %calculated breathing rate
%eHRp = load('eHRp.txt');%heart rate from Pulseoximeter

% load wild divine sensor data
%wGSR = load('Iom_GSR.txt');%  GSR from wild divine sensor
%wHR = load('Iom_BPM.txt');%  Heart rate from wild divine sensor

% load mindwave sensor data
%mStrength = load('MindStrength.txt');% signal strength
%mMed = load('Med.txt');% Medition score
%mAtten = load('Atten.txt');%Attention score

%% Sampling rate 
%fs_e = 1000;
%fs_u = 120;
%t_e = (1:length(eHealthHR))./fs_e;
%t_e = (1:length(eECG))./fs_e;
%t_u = (1:length(wGSR))./fs_u;

%% Plotting %%

% GSR Graph
column1 = eHealthGSR(:, 1);
column2 = eHealthGSR(:, 2);
column3 = eHealthGSR(:, 3);

figure('Name','Figure: eHealth Sensor','NumberTitle','off')
GSRgraph = subplot(4,1,1);
plot(column2,column1,'b', column2,column3,'r');
title('GSR');
xlabel('Time (sec)');
ylabel('\muS');
xlim([0 max(column2)]);
GSRgraph_pos = get(GSRgraph, 'Position');


% raw ECG Graph
column1 = eHealthECG(:, 1);
column2 = eHealthECG(:, 2);
column3 = eHealthECG(:, 3);

ECGgraph = subplot(4,1,2);
plot(column2,column1,'b', column2,column3,'r');
title('Raw ECG');
xlabel('Time (sec)');
ylabel('Voltage');
xlim([0 max(column2)]);
%ECGgraph_pos = get(ECGgraph, 'Position');
%y_pos = GSRgraph_pos(2)+GSRgraph_pos(4)-.45;
%set(ECGgraph, 'Position', [GSRgraph_pos(1) y_pos ECGgraph_pos(3) ECGgraph_pos(4)]);             %[x,y,width,height]


% HRV Graph
column1 = eHealthHRV(:, 1);
column2 = eHealthHRV(:, 2);
column3 = eHealthHRV(:, 3);

HRVgraph = subplot(4,1,3);
plot(column2,column1,'b', column2,column3,'r');
title('HRV');
xlabel('Time (sec)');
ylabel('HRV');
xlim([0 max(column2)]);


% Heart Beat Rate Graph
column1 = eHealthHB(:, 1);
column2 = eHealthHB(:, 2);
column3 = eHealthHB(:, 3);

HBgraph = subplot(4,1,4);
plot(column2,column1,'b', column2,column3,'r');
title('Heart Beat Rate');
xlabel('Time (sec)');
ylabel('Beats/min');
xlim([0 max(column2)]);
