var mongoose = require('mongoose');
var Schema = mongoose.Schema;

var deviceSchema = new Schema({
    name: String,
    path: String,
    ip: String,
    port: Number
})

module.exports = mongoose.model('Device', deviceSchema);