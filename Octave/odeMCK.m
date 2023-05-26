function dydt = odeMCK(t, y)
  % Parameters
    m = 0.5;% Mass = params(1)
    k = 10;% Spring constant = params(2)
    %c = 0.2204;% Damping = params(3)
    c = 0;% Damping = params(3)
    
    % Extract position and velocity from the state vector
    x = y(1);
    v = y(2);

    % Compute the derivatives
    dxdt = v;
    dvdt = -(k/m) * x - (c/m) * v;

    % Return the derivatives as a column vector
    dydt = [dxdt; dvdt];