function dydt = odeMCK2DOF(t, y)
  % Parameters
    m1 = 0.5;% Mass = params(1)
    k1 = 10;% Spring constant = params(2)
    c1 = 5.2204;% Damping = params(3)
    m2 = 0.5;% Mass = params(1)
    k2 = 10;% Spring constant = params(2)
    c2 = 5.2204;% Damping = params(3)
    %c1 = 0;% Damping = params(3)
    
    % Extract position and velocity from the state vector
    x1 = y(1);
    x2 = y(2);
    v1 = y(3);
    v2 = y(4);

    % Compute the derivatives
    dx1dt = v1;
    dx2dt = v2;
    dv1dt = -(k1+k2)/m1 * x1 + k2/m1* x2 - (c1+c2)/m1 * v1 + c2/m1 * v2;
    dv2dt = -k2/m2 * x1 - k2/m2* x2 + c2/m2 * v1 - c2/m2 * v2;

    % Return the derivatives as a column vector
    dydt = [dx1dt; dx2dt; dv1dt; dv2dt];