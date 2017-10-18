module.exports = function(context, myTimer) {
    var Twitter = require('twitter');

    var client = new Twitter({
        consumer_key: 'completar',
        consumer_secret: 'completar',
        access_token_key: 'completar',
        access_token_secret: 'completar'
    });

    var places = [{ id: 23424747, name: 'Argentina', emoji: '🇦🇷', tag: ''},
                { id: 23424768, name: 'Brasil', emoji: '🇧🇷', tag: ''},
                { id: 23424782, name: 'Chile', emoji: '🇨🇱', tag: ''},
                { id: 23424787, name: 'Colombia', emoji: '🇨🇴',  tag: ''},
                { id: 23424800, name: 'República Dominicana', emoji: '🇩🇴',  tag: ''},
                { id: 23424801, name: 'Ecuador', emoji: '🇪🇨',  tag: ''},
                { id: 23424834, name: 'Guatemala', emoji: '🇬🇹',  tag: ''},
                { id: 23424900, name: 'Mexico', emoji: '🇲🇽',  tag: ''},
                { id: 23424919, name: 'Peru', emoji: '🇵🇪',  tag: ''},
                { id: 23424935, name: 'Puerto Rico', emoji: '🇵🇷',  tag: ''},
                { id: 23424982, name: 'Venezuela', emoji: '🇻🇪',  tag: ''} 
    ];

    //NEED TO DO THIS FOR ASYNCHRONOUS FOREACH LOOP
    var processedItems = 0;

    var postTweet = function(tweet){
        client.post('statuses/update', {status: tweet}, function(error, tweet, response) {
            if (!error) {
                console.log(tweet);
            }
        });
        context.log('FUNCOOOOO');
    }

    var processTweets = function() {
        var tweet = 'Trending now in LATAM \n';
        var charCount = 22;

        for(var i = 0; i < places.length; i++){
            if((charCount + 5 + places[i].tag.length) <= 140){
                tweet += (places[i].emoji + ': ' + places[i].tag + '\n');
                charCount += (5 + places[i].tag.length);

                if(i == places.length-1){
                    postTweet(tweet);
                }
            }
            else {
                postTweet(tweet);
                tweet = 'Trending now in LATAM \n';
                var charCount = 22;
            }
        }    
    };

    var runAll = function(){
        places.forEach(function(element, i) {
            client.get('trends/place', {id: places[i].id}, function(error, tweets, response) {
                var trends = JSON.parse(response.body)[0].trends;
                var flag = false;

                trends.forEach(function(trendElement) {
                    if(!flag && trendElement.name[0] == '#') {
                        flag = true;
                        places[i].tag = trendElement.name;
                    }
                    else {}; //ForEach no admite break :(
                }, this);

                processedItems++;
                if(processedItems==places.length){
                    processTweets();
                }
            });
        }, this);
    };

    //MAGIC
    runAll();
};