clear; clear all;close all;
%%
% load E-health sensor data
eECG = load('ECG.txt');%ECG raw data
eHR = load('HR.txt');%Calculated HR using ECG signal
eHRV = load('HRV.txt');%Calculated HR using ECG signal
eGSR = load('eGSR.txt');% GSR from E-health platform
eBreathing = load('Breathing.txt');% breathing raw data
eBR = load('BR.txt');%calculated breathing rate
eHRp = load('eHRp.txt');%heart rate from Pulsiximeter
% load wild divine sensor data
wGSR = load('GSR.txt');%  GSR from wild divine sensor
wHR = load('BPM.txt');%  Heart rate from wild divine sensor

% load mindwave sensor data
mStrength = load('MindStrenth.txt');% signal strength
mMed = load('Med.txt');% Medition score
mAtten = load('Atten.txt');%Attention score

%% Sampling rate 
fs_e = 1000;
fs_u = 120;
t_e = (1:length(eECG))./fs_e;
t_u = (1:length(wGSR))./fs_u;

%% Plotting
figure(1)%Plot the ECG, HR (both from ECG and Pulsiximeter), and HRV. (from e-health platform)
subplot(4,1,1)
plot(t_e,eECG);
title('ECG raw data(e-health)');
xlim([0 max(t_e)]);
subplot(4,1,2)
plot(t_e,eHR);
title('Heart beat rate(from ECG)');
xlim([0 max(t_e)]);
subplot(4,1,3)
plot(t_e,eHRV);
title('HRV(from ECG)');
xlim([0 max(t_e)]);
subplot(4,1,4)
plot(t_e,eHRp);
title('HR(from Pulsiximeter)');
xlim([0 max(t_e)]);

%%
figure(2)% Plot GSR and HR from wild divine sensor
subplot(2,1,1)
plot(t_u,wGSR);
title('GSR(wild divine)');
xlim([0 max(t_u)]);
subplot(2,1,2)
plot(t_u,wHR);
title('Heart beat rate(wild divine)');
xlim([0 max(t_u)]);
%%
figure(3)% Plot data from mindwave sensor
subplot(3,1,1)
plot(t_u,mStrength);
title( 'Signal strength');
xlim([0 max(t_u)]);
subplot(3,1,2)
plot(t_u,mMed);
title('Medition score');
xlim([0 max(t_u)]);
subplot(3,1,3)
plot(t_u,mAtten);
title('Attention score');
xlim([0 max(t_u)]);
%%
figure(4) % HR comparsion (E-health VS Wild divine)

subplot(2,1,1)
plot(t_e,eHR);
title('Heart beat rate(E-health)');
xlim([0 max(t_e)]);
subplot(2,1,2)
plot(t_u,wHR);
title('Heart beat rate(wild divine)');
xlim([0 max(t_u)]);

%%
figure(5) % GSR comparsion (E-health VS Wild divine)

subplot(2,1,1)
plot(t_e,eGSR);
title('GSR(E-health)');
xlim([0 max(t_e)]);
subplot(2,1,2)
plot(t_u,wGSR);
title('GSR(wild divine)');
xlim([0 max(t_u)]);

figure(6)
subplot(4,1,1)
plot(t_e,eECG);
title('ECG(E-health)');
xlim([0 max(t_e)]);

subplot(4,1,2)
plot(t_e,eHR);
title('Heart beat(E-health)');
xlim([0 max(t_e)]);

subplot(4,1,3)
plot(t_e,eBR);
title('Breathing rate(E-health)');
xlim([0 max(t_e)]);

subplot(4,1,4)
plot(t_e,eGSR);
title('GSR(E-health)');
xlim([0 max(t_e)]);