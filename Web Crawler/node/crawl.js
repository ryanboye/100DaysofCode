var request = require('request');
var prompt = require('prompt');
var fs = require('fs');
var colors = require('colors');

var re = /<a[\s\S]*?href="(.*?)"/gm;
var history = {};

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
 	
 	getSite('http://leftronic.com');
});

function getSite(target){
	request(target, function (error, response, body) {
	  if (!error && response.statusCode == 200) {
	  	history[target] = 1;
	  	var log = '';
	  	var matches = getMatches(body, re, 1);
	    
	  	log += '\n\n Current Page: ' + target + '\n\n';
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
	    		if(match[0] == '/'){
	    			console.log((target + match).green);
	    			console.log(history);
	    			console.log(history[target + match] == null);
	    			if(history[target + match]  == null){
	    				getSite(target + match);
	    			}	    			
	    		}
	    	}
	    });
	  }
	})
}

