# Compact Huffman Coding

Huffman coding which is based not only on 1 character frequency, but also combinations of 2 characters.

Oriented on producing more compact results rather than speed of encoding/decoding. Which can be beneficial when sending data over the network.

Idea comes from Keyboard Layouts development, which counts not only single character usage, but also continuous pair of characters to optimize typing speed.

## Datasets

First of all I need to analyze plain english text data to figure out frequencies of single and double characters usage. I use datasets available for free, here is a list:

- [Yelp reviews](https://www.yelp.com/dataset) - informal language usage
- [Plain Text Wikipedia](https://www.kaggle.com/datasets/ltcmdrdata/plain-text-wikipedia-202011) - articles
- [Books on General Works](https://www.gutenberg.org/ebooks/results) - literature

Datasets aren't placed inside of the repository, because of licensing and other legal stuff. I may create script to download all of these datasets.

## Results

In around half of the times, my encoding solution produces smaller results

But unfortunately, average difference is only 4.2 units (which will be bits in real use scenario)

Currently, I don't think it's a good tradeoff, because calculations for my encoding are more expensive

```bash
Using frequencies from: ../datasets/results/index.json
Compact is smaller on average :)
Compact is smaller 13300 times
Compact is bigger 6693 times
Difference: 6607 times
Difference in size: 84816
Difference in size on average: 4.242284799679888
```
