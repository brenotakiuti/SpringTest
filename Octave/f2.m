function v=f2(t,x)
    m1 = 0.5;% Mass = params(1)
    k1 = 10;% Spring constant = params(2)
    c1 = 0.2204;% Damping = params(3)
    m2 = 0.5;% Mass = params(1)
    k2 = 10;% Spring constant = params(2)
    c2 = 0.2204;% Damping = params(3)
    
    M=[m1 0; 0 m2];
    K=[k1+k2 -k2; -k2 k2];
    C=[c1+c2 -c2; -c2 c2];
    B=[0; 0];
    w=2.75655;
    A1=[zeros(2) eye(2); -inv(M)*K -inv(M)*C];
    f=inv(M)*B;
    v=A1*x+[0;0; f]*sin(w*t);
end
