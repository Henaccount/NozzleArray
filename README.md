# AutoCAD Plant 3D, Create several equipment nozzles at the same time - Sample Code - Use at own risk!

<li>load compiled dll with "netload" in command line</li>
<li>execute "NozzleArray" command in command line</li>
<li>you will be prompted for input string (input including double quotes)</li><br>
<img src="https://github.com/Henaccount/NozzleArray/blob/master/nozzleArry-Input.png"><br>
<li>you will then be prompted for number of nozzles and the distance. Distance can be negative.</li>
<li>limited to StraightNozzle,Radial</li>
<li>based on example from Plant 3D SDK</li>


for copy/paste:
//metric/mixedmetric project:
//sample input: "StraightNozzle,Radial,100,FL,mm,10,C,DIN 2632,H=100,L=75,A=0,O=0,I=0,N=0,T=0"
//sample input: "StraightNozzle,Radial,4,FL,in,300,RF,substrLongDesc,H=100,L=75,A=0,O=0,I=0,N=0,T=0"
//imperial project:
//sample input: "StraightNozzle,Radial,100,FL,mm,10,C,DIN 2632,H=4,L=4,A=0,O=0,I=0,N=0,T=0"
//sample input: "StraightNozzle,Radial,4,FL,in,300,RF,substrLongDesc,H=4,L=4,A=0,O=0,I=0,N=0,T=0"
