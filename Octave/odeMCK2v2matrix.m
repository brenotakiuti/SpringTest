function v=odeMCK2v2matrix(t,x)    my = 0.5;    ky = 10;    %cy = 0.308;
    cy = 0;
    r = 0.3;    Jz = (pi * r^4) / 4;
    ktz = ky*2;
    ctz = cy*2;    xx = [2 5 11 7];
    % x must be of the form:
    % x = [y; theta; y'; theta'] 
    
    %% Coupled y displacement with z torsion    M=[my 0; 0 Jz];    K=[ky 0; 0 ktz];    C=[cy 0; 0 ctz];    B=[-9.81; 0];    %w=2.75655;    A1=[zeros(2) eye(2); -inv(M)*K -inv(M)*C]
    A1*xx'    %f=inv(M)*B;    f=M*B;    v=A1*x+[0;0; B];end