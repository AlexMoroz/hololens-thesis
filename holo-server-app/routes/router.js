var express = require('express');
var router = express.Router();
var config = require('../config/config.js');
var jwt = require('jsonwebtoken');
var User = require('../models/user.model.js');
var Device = require('../models/device.model');
var Access = require('../models/access.model');

// ---------------------------------------------------------
// authentication (no middleware necessary since this isnt authenticated)
// ---------------------------------------------------------
// http://localhost:8080/api/authenticate
router.post('/authenticate', function (req, res) {
    // find the user
    User.findOne({
        name: req.body.name
    }, function (err, user) {
        if (err) throw err;
        if (!user) {
            res.json({ success: false, message: 'Authentication failed. User not found.' });
        } else if (user) {
            // check if password matches
            user.comparePassword(req.body.password, function (err, match) {
                if (!match) {
                    res.json({ success: false, message: 'Authentication failed. Wrong password.', error: err });
                } else {
                    // if user is found and password is right
                    // create a token
                    var payload = {
                        admin: user.admin,
                        userId: user._id
                    }
                    var token = jwt.sign(payload, config.secret, {
                        expiresIn: 86400 // expires in 24 hours
                    });
                    res.json({
                        success: true,
                        message: 'Enjoy your token!',
                        token: token
                    });
                }
            });
        }
    });
});

router.get('/users', function(req, res) {
    User.find().select('name').exec(function(err, users) {
        if(!err) {
            var nameList = [];
            for(userId in users) {
                nameList.push(users[userId].name);
            }
            res.send(nameList);
        }
    })
});

// ---------------------------------------------------------
// route middleware to authenticate and check token
// ---------------------------------------------------------
router.use(function (req, res, next) {
    // check header or url parameters or post parameters for token
    var token = req.body.token || req.params['token'] || req.headers['x-access-token'];
    // decode token
    if (token) {
        // verifies secret and checks exp
        jwt.verify(token, config.secret, function (err, decoded) {
            if (err) {
                return res.json({ success: false, message: 'Failed to authenticate token.' });
            } else {
                // if everything is good, save to request for use in other routes
                req.decoded = decoded;
                 next();
            }
        });
    } else {
        // if there is no token
        // return an error
    return res.status(403).send({
            success: false,
            message: 'No token provided.'
        });
    }
    
});

router.get('/devices', function (req, res, next) {
    Access.find(function(err, devices){
        if(err) {
            res.status(500).send({message: "Some error occurred while retrieving devices."});
        } else {
            Access.findOne({"user":req.decoded.userId})
            .populate("devices")
            .exec(function(err, access){
                if(err) next(err);
                if(access != null) {
                res.send(access.devices);
                } else {
                    res.send([]);
                }
            }); 
        }
    });
})

router.get('/dataset/xml', function (req, res, next) {
    var file = './database/database.xml';
    res.download(file);
})

router.get('/dataset/dat', function (req, res, next) {
    var file = './database/database.dat';
    res.download(file);
})


module.exports = router;