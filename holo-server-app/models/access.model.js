var mongoose = require('mongoose');
var Schema = mongoose.Schema;

var accessSchema = new Schema({
    user: {
        type: mongoose.Schema.Types.ObjectId,
        ref: 'User'
    },
    devices: [{
        type: mongoose.Schema.Types.ObjectId,
        ref: 'Device'
    },]
})

module.exports = mongoose.model('Access', accessSchema);