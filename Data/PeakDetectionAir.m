clear; clear all;close all;
data1 = load('E:\Breathing.txt');

% data1 = medfilt1(data1,50);
peakAG = 0;
sample = 0;
attack = 0.9875;
decay = 0.992;

lower_bound = 0.9975;
upper_bound = 0.99;
near_peak = 0;

gain = 0;
sampleAG = 0;
j = 1;

PeakFlag = 0;
maxBufferCounter = 1;

%%
% data1 = data1 * 100;
% for i = 6 : length(data1)
%     data1 (i,1) = mean (data1(i-5:i-1,1));
% end

%%

AirCounter = 1;
for i = 1 : length(data1) 
            sample = data1(i,1);
%             maxBuffer (1,maxBufferCounter) = sample;
%             maxBufferCounter = maxBufferCounter + 1;            
%             if (maxBufferCounter == 401)
%                 maxBufferCounter = 1;
%             end
%             if (sample> max(maxBuffer)*0.5)
%                 PeakFlag = 1;
%             else 
%                 PeakFlag = 0;
%             end     
            AirBuffer(1,AirCounter) = sample;
            AirCounter = AirCounter + 1;
            if (AirCounter == 50)
                AirCounter = 1;
            end
            sample1 = mean(AirBuffer);
            data2(i,1) = sample1;
            if (sample1 > peakAG)
                peakAG = attack * sample1;
            else
                peakAG = decay * peakAG;
            end
			gain = attack / peakAG;
			sampleAG = gain * sample1;
            if (sampleAG >= lower_bound)
                near_peak = 1;
            end          
            if ((near_peak == 1) && (sampleAG < upper_bound))% && (PeakFlag == 1)
                near_peak = 0;
                Peakbuffer (1,j) = i;
                Peakbuffer (2,j) = sample1;
                j = j + 1;
            end    
end 


figure
subplot(2,1,1)
plot(data1);
subplot(2,1,2)
plot(data2);
hold on;
for i = 1 : length(Peakbuffer)
    plot (Peakbuffer(1,i),Peakbuffer(2,i),'r*','MarkerSize',10);
%    plot (20,3,'r*','MarkerSize',10);
    hold on;
end