% Test RKF45
clear
clc

tspan = [0, 1];
y0 = [0 1];
step = 0.1;
tol = 1e-10;
[t, y] = ode45(@ode, tspan, y0);

%%



resp = [t';y'];
%%
