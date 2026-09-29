let express = require('express')        // express 모듈 로딩
let app = express();                    // express 를 app 이름으로 정의하고 사용

app.get('/', function(req, res){        // 기본 라우터에서 Hello, World!를 출력
    res.send('Hello, World!');
});

app.get('/about', function(req, res){     // About 라우터에서 About Page를 출력
    res.send('About Page');
});

app.listen(3000, function() {
    console.log('listening on port 3000');
});