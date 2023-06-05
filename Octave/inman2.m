clear all
clc
close all

xo=[0.5; 0.5; 0; 0];
ts=[0 60];
[t,x]=ode45(@f2,ts,xo);
plot(t,x(:,1),t,x(:,2),'--')

