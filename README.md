# HanabiCardGame
Simplified Hanabi Card Game. 

There are two players, which has one goal - to fold all cards from hand using specific rules, which you could read on the following link: https://en.wikipedia.org/wiki/Hanabi_(card_game)

How to play:</br>
<i>Start new game with command <b>Start new game with deck DECK</b></i></br>
<i>Play card with command <b>Play card CARD_NUMBER"</b></i></br>
<i>Drop card with command <b>Drop card CARD_NUMBER"</b></i></br>
<i>Tell rank to your teammate with command <b>Tell rank RANK for cards NUMBER_OF_CARDS</b></i></br>
<i>Tell color to your teammate with command <b>"Tell color COLOR for cards NUMBER_OF_CARDS</b></i></br>

</br>
<b>RANK</b> - is a number from 1 to 5</br>
<b>COLOR</b> - one of the following colors: Red, Green, Blue, White, Yellow (you can use just the first letter for identifying the card color)</br>
<b>NUMBER_OF_CARDS</b> - numbers of neighbour cards (splitted by whitespace) that are respond to rank or color</br>
<b>DECK</b> - list of cards, splitted by whitespace using following format: R1 (red card with rank equals to 1), Y4 and so on</br>
<b>CARD_NUMBER</b> - number of card in your hand, which is going to be played</br>
