#!/usr/bin/env python

from multiprocessing import Pool
import os
import sys
import cv2
import math
from PIL import Image
from moviepy.editor import VideoFileClip
from tqdm import tqdm
import subprocess

def get_audio(base_filename, video_object, path):
    video_object.audio.write_audiofile(filename=f'{path}/{base_filename}_audio.mp3', logger=None)

def combine_audio_video(video_path, audio_path, og_path):
    capture = cv2.VideoCapture(og_path)
    file_extension = os.path.splitext(og_path)[1]
    fps = capture.get(cv2.CAP_PROP_FPS)
    video_path_real = video_path + "/%d.png"
    command1 = ["ffmpeg", "-framerate", str(int(fps)), "-i", video_path_real, "-codec", "copy",
                os.path.join(video_path, "combined_video_only" + file_extension)]
    subprocess.run(command1, stdout=subprocess.PIPE, stderr=subprocess.PIPE)

    command2 = ["ffmpeg", "-i", os.path.join(video_path, "combined_video_only" + file_extension), "-i", video_path + '/' + audio_path,
                "-codec", "copy", os.path.join(video_path, "encoded" + file_extension)]
    subprocess.run(command2, stdout=subprocess.PIPE, stderr=subprocess.PIPE)

def save_frame(args):
    frame, index, path = args
    img = Image.fromarray(frame, 'RGB')
    img.save(os.path.join(path, f'{index}.png'))

def get_frames(video_object, path):
    total_frames = math.floor(video_object.duration * video_object.fps)
    frame_args = [(frame, index, path) for index, frame in enumerate(video_object.iter_frames())]
    num_processes = os.cpu_count()

    with Pool(processes=num_processes) as pool:
        for progress, _ in enumerate(pool.imap_unordered(save_frame, frame_args)):
            convnum = progress + 1
            progress_percentage = min((convnum * 100) / total_frames, 100)
            tqdm.write('Progress - {:.2f}%'.format(progress_percentage))
def generateData(data):
    newdata = []
    for i in data:
        newdata.append(format(ord(i), '08b'))
    return newdata

def modifyPixel(pixel, data):
    datalist = generateData(data)
    lengthofdata = len(datalist)
    imagedata = iter(pixel)
    for i in range(lengthofdata):
        pixel = [value for value in imagedata.__next__()[:3] + imagedata.__next__()[:3] + imagedata.__next__()[:3]]
        for j in range(0, 8):
            if (datalist[i][j] == '0' and pixel[j] % 2 != 0):
                pixel[j] -= 1
            elif (datalist[i][j] == '1' and pixel[j] % 2 == 0):
                if (pixel[j] != 0):
                    pixel[j] -= 1
                else:
                    pixel[j] += 1

        if (i == lengthofdata - 1):
            if (pixel[-1] % 2 == 0):
                if (pixel[-1] != 0):
                    pixel[-1] -= 1
                else:
                    pixel[-1] += 1
        else:
            if (pixel[-1] % 2 != 0):
                pixel[-1] -= 1
        pixel = tuple(pixel)
        yield pixel[0:3]
        yield pixel[3:6]
        yield pixel[6:9]

def encoder(newimage, data):
    w = newimage.size[0]
    (x, y) = (0, 0)
    for pixel in modifyPixel(newimage.getdata(), data):
        newimage.putpixel((x, y), pixel)
        if (x == w - 1):
            x = 0
            y += 1
        else:
            x += 1

def encode(start, end, filename, frame_location):
    total_frame = end - start + 1
    try:
        with open(filename, 'r') as fileinput:
            filedata = fileinput.read()
    except FileNotFoundError:
        print("\nFile to hide not found! Exiting...")
        quit()
    datapoints = math.ceil(len(filedata) / total_frame)
    counter = start
    print("Performing Steganography...")

    for convnum in range(0, len(filedata), datapoints):
        numbering = frame_location +'/' + str(counter) + ".png"
        encodetext = filedata[convnum:convnum + datapoints]
        try:
            image = Image.open(numbering,'r')
        except FileNotFoundError:
            print("\n%d.png not found! Exiting..." % counter)
            quit()
        newimage = image.copy()
        encoder(newimage, encodetext)
        new_img_name = numbering
        newimage.save(new_img_name, str(new_img_name.split(".")[1].upper()))
        counter += 1
        progress = min(convnum + 1, len(filedata))
        progress_percentage = min((progress * 100) / len(filedata), 100)
        tqdm.write('Progress - {:.2f}%'.format(progress_percentage))

    convnum += 1
    numbering = frame_location +'/' + str(counter) + ".png"
    encodetext = "qzj"
    try:
        image = Image.open(numbering, 'r')
    except FileNotFoundError:
        print("\n%d.png not found! Exiting..." % counter)
        quit()
    newimage = image.copy()
    encoder(newimage, encodetext)
    new_img_name = numbering
    newimage.save(new_img_name, str(new_img_name.split(".")[1].upper()))
    counter += 1

def get_base_filename(file_path):
    return os.path.splitext(os.path.basename(file_path))[0]

def get_last_frame(files):
    img_files = [file for file in files if file.endswith(".png")]
    if img_files:
        sorted_files = sorted(img_files)
        last_file = os.path.splitext(sorted_files[-1])[0]
        return int(last_file)
    return None

def main():
    args = sys.argv
    if len(args) >= 4:
        videoname = args[1]
        filename = args[2]
        directory = args[3]
    os.makedirs(directory)

    base_filename = get_base_filename(videoname)
    video_object = VideoFileClip(videoname)
    get_audio(base_filename, video_object, directory)
    get_frames(video_object, directory)

    while True:
        try:
            frame_end = get_last_frame(os.listdir(directory))
            if frame_end is not None:
                frame_start = 0
                if frame_start < frame_end:
                    break
                else:
                    print("\nStarting Frame must be larger than ending Frame! Please try again...")
            else:
                print("\nNo frame files found. Please try again...")
        except ValueError:
            print("\nInteger expected! Please try again...")

    encode(frame_start, frame_end, filename, directory)

    audio_file = f'{base_filename}_audio.mp3'
    combine_audio_video(directory, audio_file, videoname)

    for file_name in os.listdir(directory):
        if file_name.endswith('.png'):
            try:
                os.remove(directory + '/' + file_name)
            except OSError:
                pass
    file_name = "combined_video_only"
    files = os.listdir(directory)
    for file in files:
        if file_name in file:
            os.remove(directory + '/' + file)
    os.remove(directory + '/' + audio_file)
    print("Complite: OUTPUT - encoded" + os.path.splitext(videoname)[1])

if __name__ == "__main__":
    main()