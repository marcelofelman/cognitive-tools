var request = require('request');

var queries = [
    'quiero cambiar cyber shot',
    'donde hay lente',
    'no me llega handycam',
    'cuanto sale action cam',
    'donde hay accesorio video',
    'como pago blu ray bdp s6500',
    'perdi mi blu ray bdp s1500'
];

var delay = 500;  //Un pequeno delay de 500ms para no sobrecargar a LUIS API (throttling)

queries.forEach(function(element) {
    var queryInUrl = element.replace(' ','%20');
    var qs = 'https://api.projectoxford.ai/luis/v2.0/apps/{APP_ID}?subscription-key={SUBSCRIPTION_KEY}&q={query}&verbose=true'.replace('{query}',element)

    setTimeout(function() {
        request(qs, function (error, response, body) {
        if (!error && response.statusCode == 200) {
            var luisResponse = JSON.parse(body);
            console.log('Query: ' + element + ' ; ' + 'Intent: ' + JSON.stringify(luisResponse['intents'][0]));
            }
        })
    }, delay+=500);
}, this);


