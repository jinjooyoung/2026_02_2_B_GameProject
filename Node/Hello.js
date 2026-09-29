// HTTP 모듈 로딩

let http = require('http');

http.createServer(function (req, res)
{
    res.writeHead(200, {'Content-type' : 'text/plain'});
    res.end('Hello, World!');
}).listen(8000);

console.log("Server running");