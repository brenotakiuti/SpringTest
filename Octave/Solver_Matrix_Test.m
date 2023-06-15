clear all
clc
close all


tspan = [0, 20];
y0 = [0.5 0 0 0];
h = 0.005;
tol = 1e-10;

% Solução por RungeKutta45
odestruct = odeset('AbsTol',tol, 'InitialStep', h);
[t0, y0] = ode45(@odeMCK2v2matrix, tspan, y0,odestruct);
t0 = t0';
y0 = y0';
%t = linspace(tspan(1),tspan(2),n);
%y(1,:) = interp1(t0,y0(1,:),t,"spline");
%y(2,:) = interp1(t0,y0(2,:),t,"spline");

%[t,x]=ode45(@f2,tspan,y0);
%plot(t,y(1,:),'b')
%hold on 
