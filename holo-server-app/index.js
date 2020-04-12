var express = require('express');
var bodyParser = require('body-parser');
var config = require('./config/config.js');
var mongoose = require('mongoose');
var morgan = require('morgan');


// create express app
var app = express();

app.use(express.static(__dirname + '/public')); 

// use morgan to log requests to the console
app.use(morgan('dev'));

// parse requests of content-type - application/x-www-form-urlencoded
app.use(bodyParser.urlencoded({ extended: true }))

// parse requests of content-type - application/json
app.use(bodyParser.json())

mongoose.connect(config.url);

mongoose.connection.on('error', function () {
    console.log('Could not connect to the database. Exiting now...');
    process.exit();
});

mongoose.connection.once('open', function () {
    console.log("Successfully connected to the database");
});

// include routes
var routes = require('./routes/router');
app.use('/api', routes);

// application -------------------------------------------------------------
app.get('*', function(req, res) {
    res.sendFile('./public/index.html'); // load the single view file (angular will handle the page changes on the front-end)
});

// catch 404 and forward to error handler
app.use(function (req, res, next) {
    var err = new Error('Not Found. The API is at http://localhost:8080/api');
    err.status = 404;
    next(err);
});

// error handler
// define as the last app.use callback
app.use(function (err, req, res, next) {
    res.status(err.status || 500);
    res.send(err.message);
});

// listen for requests
app.listen(8080, function () {
    console.log("Server is listening on port 8080");
});