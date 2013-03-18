# Express.NET

Proof of concept application to mimic Express.js in ASP.NET.
Very experimental, see my blog post for more information.

## Usage

```
var app = new ExpressNetApp ();

app.Get ("/", (req, res) => {
    res.Send ("hello world");
});

```

## License

(c) 2013 Kalman Speier

Licensed under the MIT License: http://www.opensource.org/licenses/mit-license.php
