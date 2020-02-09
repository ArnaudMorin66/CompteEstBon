param(
	[string] $version="17.4.47"
)
Remove-Item ./src/ej2.min.js
Remove-Item ./src/ejs.interop.min.js 
Invoke-WebRequest "https://cdn.syncfusion.com/ej2/$($version)/dist/ej2.min.js"  -OutFile ./src/ej2.min.js  -Verbose	
Invoke-WebRequest "https://cdn.syncfusion.com/ej2/$($version)/dist/ejs.interop.min.js"  -OutFile ./src/ejs.interop.min.js  -Verbose
Copy-Item 	.\src\ej2.min.js -Destination .\wwwroot\lib
Copy-Item 	.\src\ejs.interop.min.js  -Destination .\wwwroot\lib