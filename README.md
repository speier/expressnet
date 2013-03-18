# Express.NET

Proof of concept application to mimic Express.js in ASP.NET.

Very experimental, see my **upcoming** blog post for more information...

## Usage

```
var app = new Express ();

app.Get ("/", (req, res) => {
    res.Send ("hello world");
});

```

For more usage samples, please see the included [Global.asax.cs](Global.asax.cs)

## License

(c) 2013 Kalman Speier

Licensed under the MIT License: http://www.opensource.org/licenses/mit-license.php
