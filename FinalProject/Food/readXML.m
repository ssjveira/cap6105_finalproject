
filename = 'D:\food_samples\samplesALL.xml'
% PARSEXML Convert XML file to a MATLAB structure.
try
   tree = xmlread(filename)
catch
   error('Failed to read XML file %s.',filename);
end
