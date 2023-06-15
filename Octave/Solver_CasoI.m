%% Comparação de resultados da solução do ODE de um
% massa mola amortecedor 

%% Caso I: com um deslocamento inicial equivalente 
% ao processo de acomodação da massa na mola.

clear
clc
%% Preprocessamento dos dados do UNITY
sensorOffset = 1.9559; 

%Este offset existe porque o sensor utilizado para medir o deslocamento mede a distância da massa até o solo, a partir do ponto mais baixo do bloco:
% -----
%|     |
%|     |
% -----  ---
%  | |    |
%  | |    | distancia medida  
%__|_|____V_

data = load('-ascii','data.txt');
y_unity = data(:,2)-(sensorOffset);
t_unity = data(:,1);
n = length(t_unity);

tspan = [0, t_unity(end)];
y0 = [0.5 0];
h = t_unity(2)-t_unity(1);
tol = 1e-10;

% Solução por RungeKutta45
odestruct = odeset('AbsTol',tol, 'InitialStep', h);
[t0, y0] = ode45(@odeMCK, tspan, y0,odestruct);
t0 = t0';
y0 = y0';
t = linspace(tspan(1),tspan(2),n);
y(1,:) = interp1(t0,y0(1,:),t,"spline");
y(2,:) = interp1(t0,y0(2,:),t,"spline");

% Solução padrão (livro) (considerada como referência)
m = 0.5;
k = 10;
c = 0.2204;
wn = sqrt(k / m);
zeta = c / (2 * sqrt(m * k)) ;
wd = wn * sqrt(1 - zeta^2);
ya = e.^(-zeta * wn * t).*(0.5 * cos(wd * t) - zeta * 0.5 * sin(wd * t));
% Através de testagem empírica, foi determinado um amortecimento global 
% Inerente ao UNITY no valor de aproximadamente zeta = 0.05.


plot(t,y(1,:),'b')
hold on
plot(t,ya, '--r')
plot(t_unity,y_unity, 'g')
axis ([0 20 -0.50 0.55])

%% Cálculo da diferença entre sinais
R2 = abs(ya-y_unity')./ya;
MAPE_unity = 100*sum(R2)/n;

R3 = abs(ya-y(1,:))/ya;
MAPE_rk = 100*sum(R3)/n;

%%
correlation1 = corrcoef(ya,y_unity');