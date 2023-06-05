clear all
clc
close all

xo=[0; 0; 0; 0];
ts=[0 20];
[t,x]=ode45(@f,ts,xo);
plot(t,x(:,1),t,x(:,2),'--')
%-------------------------------------------
