#!!!!!!!!!!                       Автор -  https://github.com/3noth                       !!!!!!!!!!
#!/usr/bin/env python

import re
import sys
import os
from PIL import Image
from moviepy.editor import VideoFileClip

def get_frames(video_object, path):
    data = ''
    for index, frame in enumerate(video_object.iter_frames()):
        img = Image.fromarray(frame, 'RGB')
        img.save(f'{path}/{index}.png')
        if "qzj" not in data:
            data = decode(f'{path}/{index}.png', data)
        else:
            break
    with open(f'{path}/decoded.txt', 'a') as decodedtextfile:
        decodedtextfile.write(data[:-3])


def decode(img_path, data):
    image = Image.open(img_path, 'r')
    imagedata = iter(image.getdata())
    while True:
        pixels = [value for value in imagedata.__next__()[:3] + imagedata.__next__()[:3] + imagedata.__next__()[:3]]
        binstr = ''.join(['0' if i % 2 == 0 else '1' for i in pixels[:8]])
        if re.match("[ -~]", chr(int(binstr, 2))) is not None:
            data += chr(int(binstr, 2))
        if pixels[-1] % 2 != 0:
            return data


if __name__ == "__main__":
    if len(sys.argv) >= 3:
        videoname = sys.argv[1]
        directory = sys.argv[2]
        os.makedirs(directory, exist_ok=True)
        video_object = VideoFileClip(videoname)
        get_frames(video_object, directory)

        files_in_dir = os.listdir(directory)
        for file_name in files_in_dir:
            if file_name.endswith('.png'):
                try:
                    os.remove(os.path.join(directory, file_name))
                except OSError:
                    pass
        print("Complete: OUTPUT - decoded.txt")