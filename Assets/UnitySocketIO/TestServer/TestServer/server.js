var port = process.env.PORT || 3000,
    io = require('socket.io')(port),
    gameSocket = null;

gameSocket = io.on('connection', function(socket){
    console.log('socket connected: ' + socket.id);

    socket.on('disconnect', function(){
        console.log('socket disconnected: ' + socket.id);
    });

    socket.on('test-event1', function(){
        console.log('got test-event1');
    });

    socket.on('test-event2', function(data){
        console.log('got test-event2');
        console.log(data);

        socket.emit('test-event', {
            test:12345,
            test2: 'test emit event'
        });
    });

    socket.on('test-event3', function(data, callback){
        console.log('got test-event3');
        console.log(data);

        callback({
            test: 123456,
            test2: "test3"
        });
    });


});