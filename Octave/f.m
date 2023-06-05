function v=f(t,x)
M=[4 0; 0 9];
K=[30 -5; -5 5];
B=[0.23500; 2.97922];
w=2.75655;
A1=[zeros(2) eye(2); -inv(M)*K zeros(2)];
f=inv(M)*B;
v=A1*x+[0;0; f]*sin(w*t);
end

