const {add} = require("./Math.js");   // Math.js에서 add 함수를 가져옴

let num = 42;           // int
let name = "TOM";       // string
let isStudent = true;   // bool

// 배열
let color = ["red", "green", "blue"];
// 객체
let person = {
    name: "TOM",
    age: 20,
    isStudent: true
};

console.log(add(num,num));   // 84

// 함수
function greet(name)
{
    console.log("Hello, " + name + "!");
}
// 함수 호출
greet(person.name);

// 조건문
if (num > 30)
{
    console.log("num > 30");
}
else
{
    console.log("num <= 30");
}

// 반복문
for (let i = 0; i < 5; i++)
{
    console.log("i = " + i);
}

// 비동기 콜백
setTimeout(() => {
    console.log("Delayed Message 1");
}, 1000);   // 1초

setTimeout(() => {
    console.log("Delayed Message 2");
}, 750);   // 0.75초

setTimeout(() => {
    console.log("Delayed Message 1");
}, 2000);   // 2초

setTimeout(() => {
    console.log("Delayed Message 1");
}, 500);   // 0.5초