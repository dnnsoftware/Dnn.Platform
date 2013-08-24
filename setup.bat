cd c:\Git
"C:\Program Files (x86)\Git\cmd\git" clone -n "https://github.com/dnnsoftware/Dnn.Platform.git"
cd Dnn.Platform
"C:\Program Files (x86)\Git\cmd\git" remote add –f origin "https://github.com/dnnsoftware/Dnn.Platform.git"
"C:\Program Files (x86)\Git\cmd\git" config core.sparsecheckout true
echo /Build/*>> .git/info/sparse-checkout
echo /Dnn Platform/*>> .git/info/sparse-checkout
echo /Packages/*>> .git/info/sparse-checkout
echo /Website/*>> .git/info/sparse-checkout
echo /DNN_Platform.sln>> .git/info/sparse-checkout
"C:\Program Files (x86)\Git\cmd\git" checkout
