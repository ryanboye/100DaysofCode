var request = require('request');
var prompt = require('prompt');
var fs = require('fs');
var colors = require('colors');

var re = /<a[\s\S]*?href="(.*?)"/gm;
var slash_re = /\/{2,}/gm;
var history = {};


fs.writeFileSync('log.txt', '');
fs.writeFileSync('history.txt', '');

function getMatches(string, regex, index) {
  index || (index = 1); // default to the first capturing group
  var matches = [];
  var match;
  while (match = regex.exec(string)) {
    matches.push(match[index]);
  }
  return matches;
}

//
// Start the prompt
//
prompt.start();

//
// Get two properties from the user: username and email
//
prompt.get(['target'], function (err, result) {
  //
  // Log the results.
  //
  console.log('Command-line input received:');
  console.log('Target' + result.target);
 	
 	getSite(result.target);
});

function getSite(target){
	request(target, function (error, response, body) {
		if (error || response.statusCode != 200){
			console.log('REQUEST ERROR: '.red + target);
			fs.appendFile('errors.txt', error + ' ' + response.statusCode + '\n', function(err) {
			  if (err) throw err;
			});
		}
	  if (!error && response.statusCode == 200) {
	  	history[target] = 1;
	  	fs.appendFile('history.txt', target + '\n', function(err) {
			  if (err) throw err;
			});
	  	var log = '';
	  	var matches = getMatches(body, re, 1);
	    
	  	log += '\n\nCurrent Page: ' + target + '\n\n';
	  	log += 'Matches: ' + '\n';

	  	matches.forEach(function(match){
	    	log += match + '\n';
	    });

	  	log += '\n\n -------------------------------------------------------- \n\n';

	  	fs.appendFile('log.txt', log, function(err) {
			  if (err) throw err;
			});
	  	
	    // console.log(matches) // Show the HTML for the Google homepage.

	    matches.forEach(function(match){
	    	if(match.length > 2){
	    		// console.log(match);

	    		//Internal site match
	    		if(match[0] == '/'){	    			
	    			var newTarget = 'http://' + (target + match).replace('http://', '').replace(slash_re, '/');	    			
	    			// console.log((target + match).green);
	    			// console.log(history);
	    			if(history[newTarget] == null){
	    				console.log('NEW: '.green + (newTarget));
	    			}else if (history[newTarget] == 1){
	    				console.log('EXISTS: '.yellow + (newTarget));
	    			}
	    			if(history[newTarget]  == null){
	    				getSite(newTarget);
	    			}	    			
	    		}

	    		// external site yeeee
	    		if(match.indexOf('http://') == '0'){	    			
	    			var newTarget = match;	    			
	    			if(history[newTarget] == null){
	    				console.log('NEW: '.green + (newTarget));
	    			}else if (history[newTarget] == 1){
	    				console.log('EXISTS: '.yellow + (newTarget));
	    			}
	    			if(history[newTarget]  == null){	    				
	    				getSite(newTarget);
	    			}	    			
	    		}
	    	}
	    });
	  }
	})
}

