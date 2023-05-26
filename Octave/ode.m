function dy=ode(t,y)
  x = y(1);
  v = y(2);
  
  dxdt = v;
  dvdt = -2*v-2*x;
  
  dy = [dxdt; dvdt];
endfunction