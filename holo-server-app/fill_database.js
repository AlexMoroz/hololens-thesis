var mongoose = require('mongoose');
var config = require('./config/config.js');
var User = require('./models/user.model.js');
var Device = require('./models/device.model');
var Access = require('./models/access.model');

mongoose.connect(config.url);

mongoose.connection.on('error', function () {
    console.log('Could not connect to the database. Exiting now...');
    process.exit();
});

mongoose.connection.once('open', function () {
    console.log("Successfully connected to the database");
})

/** Create users */
new User({
    name: 'admin',
    password: 'admin',
    admin: true
}).save(function (err, user) {
    if (err) throw err;
    console.log('User ' + user.name + ' successfully');

    /** Create devices */
    new Device({
        name: "test_device",
        path: "api/info",
        ip: "192.168.0.1",
        port: "8080"
    }).save(function (err, device) {
        if (err) throw err;
        console.log('Device ' + device.name + ' saved successfully');

        /** Create acess rights */
        Device.find(function (err, devices) {
            User.findOne({ 'name': 'admin' }, function (err, user) {
                new Access({
                    user: user,
                    devices: devices
                }).save(function (err, device) {
                    if (err) throw err;
                    console.log('User ' + user.name + ' has now access for all devices');
                })
            });
        });
    });
});