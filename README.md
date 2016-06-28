# HanabiCardGame
Simplified Hanabi Card Game. 

There are two players, which has one goal - to fold all cards from hand using specific rules, which you could read on the following link: https://en.wikipedia.org/wiki/Hanabi_(card_game)

How to play:</br>
Start new game with command "Start new game with deck DECK"</br>
Play card with command "Play card CARD_NUMBER" </br>
Drop card with command "Drop card CARD_NUMBER"</br>
Tell rank to your teammate with command "Tell rank RANK for cards NUMBER_OF_CARDS";</br>
Tell color to your teammate with command "Tell color COLOR for cards NUMBER_OF_CARDS";</br>

RANK - is a number from 1 to 5
COLOR - one of the following colors: Red, Green, Blue, White, Yellow (you can use just the first letter for identifying the card color)
NUMBER_OF_CARDS - numbers of neighbour cards (splitted by whitespace) that are respond to rank or color
DECK - list of cards, splitted by whitespace using following format: R1 (red card with rank equals to 1), Y4 and so on
CARD_NUMBER - number of card in your hand, which is going to be played
