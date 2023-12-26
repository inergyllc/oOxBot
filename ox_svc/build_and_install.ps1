# build_and_install.ps1

# Navigate to the directory containing setup.py
cd "E:\oxai\src\OxBot\ox_svc" 

# Remove any previous build artifacts
Remove-Item .\dist -Recurse -ErrorAction Ignore

# Build the package (wheel and source distribution)
python setup.py sdist bdist_wheel

# Install or upgrade the package using the generated wheel
# Adjust the following line to match the naming pattern of your .whl file or find the .whl file automatically
$wheelFile = Get-ChildItem -Path .\dist\ -Filter ox_svc*.whl | Select-Object -First 1
pip install --upgrade $wheelFile.FullName
