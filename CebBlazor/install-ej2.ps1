param(
	[string] $version="17.4.42"
)
Invoke-WebRequest "https://cdn.syncfusion.com/ej2/$($version)/dist/ej2.min.js"  -OutFile ./src/ej2.min.js  -Verbose	
Invoke-WebRequest "https://cdn.syncfusion.com/ej2/$($version)/dist/ejs.interop.min.js"  -OutFile ./src/ejs.interop.min.js  -Verbose
