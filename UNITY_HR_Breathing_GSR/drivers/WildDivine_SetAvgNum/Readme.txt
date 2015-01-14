test.cs:  GUI testing sample
lightstone.dll: our library, you can place it anywhere and remember the path

We will use lightstone.dll to read data,so in C# code we want to import the lightstone.dll file. The code is as follow: 

[DllImport(@"C:\Users\azhang\Desktop\WildDivine\lightstone.dll")]


In above code, my lightstone.dll path is "C:\Users\azhang\Desktop\WildDivine\", please change the path in your own PC, Then the new code should be:


[DllImport(@"your path\lightstone.dll")]

There are three places needed to be changed in total.


Since we add skin data , we update our library file to "lightstone_scl.dll". Its usage is:
[DllImport("C:/Users/azhang/Desktop/WildDivine/lightstone_scl.dll")]
extern static float lightstone_Readscl();


We add function lightstone_SetAvgNum to set the peak numbers to do average.