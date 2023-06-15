%% Compara��o de resultados da solu��o do ODE de um
% massa mola amortecedor 

% Resultado: 
clear  all
clc
close  all

%% Caso II: com um deslocamento inicial equivalente 
%  ao processo de acomoda��o da massa na mola virado 
%  para baixo

%% Preprocessamento dos dados do UNITY
sensorOffset1 = 2.950-0.5;
sensorOffset2 = 5.950-0.5;
##sensorOffset1 = 2.9559-0.5;
##sensorOffset2 = 5.9559-0.5;

data1 = load('-ascii','data21.txt');
data2 = load('-ascii','data22.txt');
y_unity1 = data1(:,2)-(sensorOffset1);
y_unity1 = y_unity1 *(-1);
y_unity2 = data2(:,2)-(sensorOffset2);
y_unity2 = y_unity2 *(-1);
t_unity = data1(:,1);
n = length(t_unity);

tspan = [0, t_unity(end)];
y0 = [0 0 0 0];
h = t_unity(2)-t_unity(1);
tol = 1e-10;

% Solu��o por RungeKutta45
odestruct = odeset('AbsTol',tol, 'InitialStep', h);
[t0, y0] = ode45(@f3, tspan, y0,odestruct);
t0 = t0';
y0 = y0';
t = linspace(tspan(1),tspan(2),n);
y(1,:) = interp1(t0,y0(1,:),t,"spline");
y(2,:) = interp1(t0,y0(2,:),t,"spline");

%[t,x]=ode45(@f2,tspan,y0);
plot(t,y(1,:),'b')
hold on 
plot(t_unity,y_unity1, 'r--')
axis ([0 t_unity(end) -2 0.2])
figure()
plot(t,y(2,:),'b')
hold on 
plot(t_unity,y_unity2, 'r--')
axis ([0 t_unity(end) -3 0.2])


%% C�lculo da diferen�a entre sinais
R1 = 100*abs(y(1,:)-1-(y_unity1'-1))./(y(1,:)-1);
MAPE_unity1 = sum(R1(2:end))/(n-1)

R2 = 100*abs(y(2,:)-1-y_unity2'+1)./(y(2,:)-1);
MAPE_unity2 = sum(R2(2:end))/(n-1)

cc1 = xcorr(y(1,:), y_unity1');
correlation_coefficient1 = max(cc1) / sqrt(sum(y(1,:).^2) * sum(y_unity1'.^2));
cc2 = xcorr(y(2,:), y_unity2');
correlation_coefficient2 = max(cc2) / sqrt(sum(y(2,:).^2) * sum(y_unity2'.^2));

% https://pt.wikipedia.org/wiki/Coeficiente_de_correla%C3%A7%C3%A3o_de_Pearson 
% 2004_Derrick_TimeSeriesAnalysis.pdf


%% Ent�o como a final � calculado esse offset?
% Diferente do que acontece nas equa��es diferenciais,
% o Unity SIMULA O TRANSIENTE do sistema. Isso quer dizer
% que mesmo n�o tendo condi��es iniciais, a massa compri-
% me o sistema de mola e amortecedor at� atingir equil�-
% brio. Dessa forma, para validar os resultados, eu consi-
% dero esse transiente como condi��es iniciais. Para isso
% eu calculo o delocamento x da mola considerando o sistema
% "est�tico". A partir desse c�lculo se obt�m:
% y0 = [1 1.5 0 0];
% Por�m, o sensor ultrass�nico n�o mede deslocamente rela-
% tivo, como na EDO. Ele calcula a dist�ncia do sensor at�
% um obst�culo, nesse caso a base de fixa��o do sistema.
% Dessa forma o offset final �: 
% erro pr�prio do sensor - deslocamento inicial (est�tico) + dist�ncia entre a massa e base
%
% *o erro do sensor est� em .5441 devido ao collider da base
% ** existe tamb�m o offset da possicao do sensor. Se considerar
% que o sensor sempre est� na superf�cie do corpo, a sua medida vai
% estar deslocada:
%
% ----
% |  |  -
% ----  |
%  |x   |x+0.501
%  |    |
%  v    v
%
% Como o cubo tem 1un de lado, a sua medi��o estar� deslocada
% em 0.5 com rela��o ao centro.