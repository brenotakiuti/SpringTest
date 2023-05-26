function [t, y] = rkf45(f, tspan, y0, h, tol)
  % RKF45 method implementaton
  % Inputs:
  %   - f: functon handle representng the system of ODEs (dy/dt = f(t, y))
  %   - tspan: tme span [t0, tf]
  %   - y0: inital conditon at t0
  %   - h: step size
  %   - tol: tolerance for adaptve step size control
  % Outputs:
  %   - t: vector of tme points
  %   - y: matrix of soluton vectors at corresponding tme points
  
  t0 = tspan(1);
  tf = tspan(2);
  t = [t0 t0];
  n = length(t);
  y = zeros(length(y0), n);
  y(:, 1) = y0;
  
  i = 1;  %time index
  while t(i)<=tf
    h = step;
    do
      ti = t(i);
      yi = y(:, i);
      
      % Compute the slopes
      k1 = ode(ti, yi);
      k2 = ode(ti + h/4, yi + (h/4) * k1);
      k3 = ode(ti + (3*h/8), yi + (3*h/32)*k1 + (9*h/32)*k2);
      k4 = ode(ti + (12*h/13), yi + (1932*h/2197)*k1 - (7200*h/2197)*k2 + (7296*h/2197)*k3);
      k5 = ode(ti + h, yi + (439*h/216)*k1 - 8*h*k2 + (3680*h/513)*k3 - (845*h/4104)*k4);
      k6 = ode(ti + h/2, yi - (8*h/27)*k1 + 2*h*k2 - (3544*h/2565)*k3 + (1859*h/4104)*k4 - (11*h/40)*k5);
      
      % Compute the soluton at the next tme step using the 4th and 5th order approximatons
      y4 = yi + (25*h/216)*k1 + (1408*h/2565)*k3 + (2197*h/4104)*k4 - (h/5)*k5;
      y5 = yi + (16*h/135)*k1 + (6656*h/12825)*k3 + (28561*h/56430)*k4 - (9*h/50)*k5 + (2*h/55)*k6;
      
      % Estmate the error
      error = abs(y5 - y4);
      t(i+1) = t(i)+h;
      
      % Update the step size for the next iteraton
      h = h * ((tol*h)/(2*error))^(1/4);
    until error <= tol
    y(:, i+1) = y5;
    i = i+1;  
    endwhile