var app = angular.module('myApp', ['ngCookies', 'rzModule']);

app.controller('mainController', function ($scope, $http, $cookies, $q, $interval) {
    $scope.auth = {};
    $scope.selected = [];
    $scope.devices = [];
    $scope.tab = {
        active: "main"
    };
    $scope.simulationEnabled = false;
    $scope.simulation = {
        motor: false,
        speed: 0,
        gear1: 0,
        gear2: 100,
        rotate: function () {
            var result = [];
            if ($scope.simulation.gear1 < 25) {
                result.push("Gear 1: biased to the left");
            } else if ($scope.simulation.gear1 > 30) {
                result.push("Gear 1: biased to the right");
            } else {
                result.push("Gear 1: in position");
                if ($scope.simulation.gear2 < 18) {
                    result.push("Gear 2: biased to the left");
                } else if ($scope.simulation.gear2 > 20) {
                    result.push("Gear 2: biased to the right");
                } else {
                    result.push("Gear 2: in position");
                    if ($scope.simulation.motor == false) {
                        result.push("Motor is off");
                    } else {
                        if ($scope.simulation.speed < 25 || $scope.simulation.speed > 90) {
                            sendLight("red");
                        } else if ($scope.simulation.speed < 75 || $scope.simulation.speed > 80) {
                            sendLight("yellow");
                        } else {
                            sendLight("green");
                        }
                    }
                }
            }
            return result;
        }
    }

    function sendLight(color) {
        if (color == "red") {
            color = "FF0000";
        } else if (color == "yellow") {
            color = "FFFF00";
        } else {
            color = "00FF0";
        }
        $http.post("http://10.42.0.166:5000/api/rgb/set", {
            data: color
        }).then(function (result) {
            console.log(result.data);
        });
    }



    //simulation
    $interval(function () {
        if ($scope.simulationEnabled) {
            $scope.loadingSimulation = false;
            $scope.simulationOutput = [];
            addToSimulation("Last updated on " + new Date().toLocaleString());
            var device = _.find($scope.selected, function (item) {
                return item.name == "raspberry_pi2";
            });
            if (device == undefined) {
                addToSimulation("Controller for Gear 1 not found!");
            } else {
                _.each(device.actors, function (actor) {
                    if (actor.type == "output") {
                        $scope.simulation.gear1 = actor.realValue;
                    }
                });
            }
            device = _.find($scope.selected, function (item) {
                return item.name == "raspberry_pi";
            });
            if (device == undefined) {
                addToSimulation("Controller for Gear 2 not found!");
            } else {
                _.each(device.actors, function (actor) {
                    if (actor.type == "slider") {
                        $scope.simulation.gear2 = actor.value;
                    }
                });
            }
            var array = $scope.simulation.rotate();
            _.each(array, function(string) {
                addToSimulation(string);
            });
            var device = _.find($scope.selected, function (item) {
                return item.name == "banana_pi";
            });
            if (device == undefined) {
                addToSimulation("Motor not found!");
            } else {
                _.each(device.actors, function (actor) {
                    if (actor.name == "Start" && actor.value == true) {
                        $scope.simulation.motor = true;
                        actor.value = null;
                    }
                    if (actor.name == "Stop" && actor.value == true) {
                        $scope.simulation.motor = false;
                        actor.value = null;
                    }
                    if (actor.type == "slider" && $scope.simulation.motor) {
                        $scope.simulation.speed = actor.value;
                    }
                });
                addToSimulation("Motor state: " + $scope.simulation.motor);
                if ($scope.simulation.motor) {
                    addToSimulation("Current speed: " + $scope.simulation.speed);
                }
            }
            $scope.loadingSimulation = true;
        }
    }, 2000);

    function addToSimulation(string) {
        $scope.simulationOutput.push(string);
    }

    //set up search
    loadingLocations = true;
    $http.get("names.json").then(function (result) {
        loadingLocations = false;
        $scope.names = _.sortBy(result.data, "name");
    });

    //get dummy actors
    var getDummyActors;
    $http.get("dummyData.json").then(function (result) {
        getDummyActors = function () {
            var rand = Math.floor((Math.random() * 10));
            return _.map(result.data[rand].actors, function (actor) {
                if (actor.type == "slider") {
                    actor.options = {
                        floor: 0,
                        ceil: 100,
                        step: 1,
                        hidePointerLabels: true,
                        hideLimitLabels: true
                    }
                }
                return actor;
            });
        }
    });


    function updateSearchData(list) {
        $scope.search = _.map(list, function (object) {
            var item = _.find($scope.devices, function (device) {
                return device.name == object.name;
            });
            if (item == undefined) {
                object.actors = getDummyActors();
                return object;
            } else return item;
        });
    }
    $scope.inputUpdate = function (value) {
        var list = _.filter($scope.names, function (device) {
            if (!_.contains($scope.selected, device) && device.name.toLowerCase().indexOf(value.toLowerCase()) > -1) {
                return device;
            }
        });
        updateSearchData(_.first(list, 10));
    }


    // get users
    $http.get('/api/users')
        .then(function (result) {
            $scope.users = result.data;
            $scope.auth.login = result.data[0];
        }, function (data) {
            console.log('Error: ' + JSON.stringify(data));
        });

    if ($cookies.get("token") != undefined) {
        $scope.credentialsError = false;
        $scope.hideAuth = true;
        loadAll($cookies.get("token"));
    }


    $scope.addToWorkspace = function (device) {
        $scope.search.splice(_.findIndex($scope.search, {
            name: device.name
        }), 1);
        $scope.selected.push(device);
    }

    $scope.deleteFromWorkspace = function (device) {
        $scope.selected.splice(_.findIndex($scope.selected, {
            name: device.name
        }), 1);
    }

    $scope.submit = function (login, password) {
        var data = {
            name: login,
            password: password
        }
        $http.post("/api/authenticate", data)
            .then(function (result) {
                var answer = result.data;
                $cookies.put("token", answer.token);
                if (!answer.success) {
                    $scope.credentialsError = true;
                } else {
                    $scope.credentialsError = false;
                    $scope.hideAuth = true;
                    loadAll(answer.token);
                }
            }, function (data) {
                console.log('Error: ' + JSON.stringify(data));
            });
    }

    function loadAll(token) {
        //get all devices
        $http.get('/api/devices', {
            headers: {
                'x-access-token': token
            },
        }).then(function (result) {
            var answer = result.data;
            _.each(answer, function (device) {
                device.url = "http://" + device.ip + ":" + device.port + "/";
                $http.get(device.url + device.path).then(function success(result) {
                    var answer = result.data;
                    device.actors = [];
                    _.each(answer, function (info) {
                        var actor = getActor($http, info, device, $interval);
                        device.actors.push(actor);
                    });
                    console.log("Device " + device.name + " loaded");
                    $scope.devices.push(device);
                }, function error(data) {
                    console.log('Error:' + JSON.stringify(data));
                });
            });
        }, function (data) {
            console.log('Error: ' + JSON.stringify(data));
        });

    }

});

function getActor($http, info, device, $interval) {
    var actor = {};
    var request = $http.get;
    if (info.method == "POST") request = $http.post;
    switch (info.type) {
        case "output":
            {
                actor = info;
                $interval(function () {
                    request(device.url + info.path)
                        .then(function (result) {
                            var answer = result.data;
                            actor.value = "Current temperature: " + answer.data;
                            actor.realValue = answer.data;
                        }, function (data) {
                            console.log('Error: ' + JSON.stringify(data));
                        });
                }, 2000);
                break;
            }
        case "slider":
            {
                actor = info;
                actor.options = {
                    floor: 0,
                    ceil: 100,
                    step: 1,
                    hidePointerLabels: true,
                    hideLimitLabels: true,
                    onChange: function (sliderId, modelValue, highValue, pointerType) {
                        var data = {
                            data: modelValue
                        }
                        request(device.url + info.path, data)
                            .then(function (result) {
                                var answer = result.data;
                                console.log(JSON.stringify(answer));
                            }, function (data) {
                                console.log('Error: ' + JSON.stringify(data));
                            });
                    }
                };
                actor.value = 50;
                break;
            }
        case "input":
            {
                actor = info;
                actor.method = function (value) {
                    var data = {
                        data: value
                    }
                    actor.value = "";
                    request(device.url + info.path, data)
                        .then(function (result) {
                            var answer = result.data;
                            console.log(JSON.stringify(answer));
                        }, function (data) {
                            console.log('Error: ' + JSON.stringify(data));
                        });
                }
                break;
            }
        case "button":
            {
                actor = info;
                actor.value = null;
                actor.method = function () {
                    actor.value = true;
                    request(device.url + info.path)
                        .then(function (result) {
                            var answer = result.data;
                            console.log(JSON.stringify(answer));
                        }, function (data) {
                            console.log('Error: ' + JSON.stringify(data));
                        });
                }
                break;
            }
    }
    return actor;
}