function v=odeMCK2v2matrix(t,x)
    cy = 0;
    r = 0.3;
    ktz = ky*2;
    ctz = cy*2;
    % x must be of the form:
    % x = [y; theta; y'; theta'] 
    
    %% Coupled y displacement with z torsion
    A1*xx'